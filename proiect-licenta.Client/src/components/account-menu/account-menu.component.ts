import { Component } from '@angular/core';
import {Button} from 'primeng/button';
import {MetamaskLoginComponent} from '../metamask-login/metamask-login.component';
import {RouterLink} from '@angular/router';
import {Popover, PopoverModule} from 'primeng/popover';
import {AuthService} from '../../services/auth.service';
import {NgIf} from '@angular/common';
import contractData from '../../../../hardhatproj/artifacts/contracts/AppStore.sol/AppStore.json';
import {environment} from '../../environments/environment';
import {ethers} from 'ethers';
import {MessageService} from 'primeng/api';
import {Toast} from 'primeng/toast';
import {MetaMaskService} from '../../services/metamask.service';

@Component({
  selector: 'app-account-menu',
  imports: [
    Button,
    MetamaskLoginComponent,
    RouterLink,
    Popover,
    Toast
  ],
  templateUrl: './account-menu.component.html',
  styleUrl: './account-menu.component.scss'
})
export class AccountMenuComponent {

  isDisabled: boolean = false;

  constructor(private authService: AuthService, private messageService: MessageService,
              private metaMaskService: MetaMaskService) {}

  async withdraw() {
    this.isDisabled = true;
    try{
      // should send transaction from here?
      // make a verification and a record of withdrawal?
      // but get functionality first
      const abi = contractData.abi;
      const contractAddress = environment.liveContractAddr;
      //const contractAddress = environment.localContractAddr;

      let signer = null;
      let provider;
      if ((window as any).ethereum == null) {
        console.log("MetaMask not installed, using defaults");
        provider = ethers.getDefaultProvider();
      } else {
        // for live
        provider = new ethers.BrowserProvider((window as any).ethereum);
        // for local
        //provider = new ethers.JsonRpcProvider("http://127.0.0.1:8545");

        signer = await provider.getSigner();
      }

      const existingWalletAddress = await this.metaMaskService.getWallet();
      const walletAddress = await signer!.getAddress();

      if (existingWalletAddress.toLowerCase() != walletAddress.toLowerCase()){
        this.messageService.add({severity: 'error', summary: 'Incorrect wallet used for payment, please change Metamask wallet'});
        throw new Error('Incorrect wallet address');
      }

      interface AppStoreContract {
        withdraw(): Promise<ethers.TransactionResponse>;
      }

      console.log("calling contract...");

      const contract = new ethers.Contract(contractAddress, abi, signer) as unknown as AppStoreContract;
      const tx = await contract.withdraw();
      const receipt = await tx.wait();

      //log withdrawal here - as payment record? doesn't make much sense tho

      //nothing else?
    } catch (error: any) {
      console.log("Error purchasing app:" + error.message);
      this.isDisabled = false;
      this.messageService.add({severity: 'error', summary: 'Error withdrawing funds:' + error.message});
    }

    this.isDisabled = false;
  }

}
