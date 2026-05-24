import { Component } from '@angular/core';
import {MetaMaskService} from '../../services/metamask.service';
import {HttpClient} from '@angular/common/http';
import {NgIf} from '@angular/common';
import {environment} from '../../environments/environment.development';
import {Button} from 'primeng/button';

@Component({
  selector: 'app-metamask-login',
  imports: [
    NgIf,
    Button
  ],
  templateUrl: './metamask-login.component.html',
  styleUrl: './metamask-login.component.scss'
})
export class MetamaskLoginComponent {
  messageDisplay: string | null = null;
  isDisabled: boolean = false;

  constructor(private metaMaskService: MetaMaskService, private http: HttpClient) {}

  async connect() {
    this.isDisabled = true;
    this.messageDisplay = await this.metaMaskService.connectWallet();
    this.isDisabled = false;
  }

}
