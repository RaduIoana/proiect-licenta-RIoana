import { Injectable } from '@angular/core';
import Web3 from 'web3';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {environment} from '../environments/environment';
import {ethers} from 'ethers';
import {firstValueFrom, Observable} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MetaMaskService {
  private readonly web3!: Web3;

  constructor(private http: HttpClient) {
    if ((window as any).ethereum) {
      this.web3 = new Web3((window as any).ethereum);
    } else {
      console.error("MetaMask not found. Please install it.");
    }
  }

  async connectWallet(): Promise<string | null> {
    try {
      if (!this.web3) return "Metamask is not installed/activated on your browser.";

      let wallet = await this.getWallet();
      if(wallet != "") return "You've already signed in with MetaMask.";

      const accounts = await (window as any).ethereum.request({method: 'eth_requestAccounts'});
      console.log(accounts);
      wallet = accounts[0];

      const message = "App store authentication attempt.";
      const provider = new ethers.BrowserProvider((window as any).ethereum);
      const signer = provider.getSigner();
      const signature = await this.signMessage(message, wallet);

      const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
      const postResponse = await firstValueFrom(this.http.post(`${environment.apiUrl}/api/MetaAuth/verify`,
        {
          wallet,
          signature,
          message
        },
        {headers}));
      console.log(`Signed in successfully.`);
      return "Signed in successfully.";
    } catch (error) {
      console.error("User denied MetaMask connection", error);
      return "Error signing in to Metamask.";
    }
  }

  async signMessage(message: string, wallet: string | null): Promise<string | null> {
    if (!wallet || !this.web3) return null;

    try {
      return await this.web3.eth.personal.sign(message, wallet, '');
    } catch (error) {
      console.error("Signing failed", error);
      return null;
    }
  }

  async getWallet(): Promise<string> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom( this.http.get(`${environment.apiUrl}/api/MetaAuth/getWallet`, {headers, responseType: "text"}));
  }
}
