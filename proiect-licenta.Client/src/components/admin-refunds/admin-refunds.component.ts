import { Component } from '@angular/core';
import {Router} from '@angular/router';
import {Refund, RefundService} from '../../services/refund.service';
import {MessageService} from 'primeng/api';
import {Button} from 'primeng/button';
import {DatePipe, NgForOf} from '@angular/common';
import { TableModule } from 'primeng/table';
import {Toast} from 'primeng/toast';
import {Tag} from 'primeng/tag';

@Component({
  selector: 'app-admin-refunds',
  imports: [
    Button,
    TableModule,
    DatePipe,
    Toast,
    Tag
  ],
  templateUrl: './admin-refunds.component.html',
  styleUrl: './admin-refunds.component.scss'
})
export class AdminRefundsComponent {
  refunds: Refund[] = [];
  disableButton: boolean = false;

  constructor(private refundService: RefundService, private messageService: MessageService) {
    this.refundService.getAllRefundsAdmin().subscribe({
      next: (data) => {
        this.refunds = data;
      },
      error: (err) => {
        console.error('Error fetching refunds', err);
        this.messageService.add({severity: 'error', summary: 'Error', detail: 'Error fetching refunds'});
      }
    });
  }

  async grantRefund(refundId: number) {
    try{
      this.disableButton = true;
      await this.refundService.grantRefund(refundId);
      this.messageService.add({severity: 'success', summary: 'Error', detail: 'Refund successful'});
      this.disableButton = false;
      window.location.reload();
    } catch(error){
      this.messageService.add({severity: 'error', summary: 'Error', detail: 'Error granting refund'});
      this.disableButton = false;
    }
  }

  determineTagSeverity(status: string) {
    switch (status) {
      case 'Complete':
        return 'success';
      case 'Processing':
        return 'warn';
      case 'Denied':
        return 'danger';
      default:
        return 'info';
    }
  }
}
