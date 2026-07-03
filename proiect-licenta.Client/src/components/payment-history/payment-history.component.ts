import { Component } from '@angular/core';
import {NgForOf, NgIf} from "@angular/common";
import {Router} from '@angular/router';
import {PaymentRecord, PaymentRecordsService} from '../../services/payment-records.service';
import {Button} from 'primeng/button';
import {MessageService} from 'primeng/api';
import {RefundService} from '../../services/refund.service';
import {Tag} from 'primeng/tag';
import {AuthService} from '../../services/auth.service';

@Component({
  selector: 'app-payment-history',
  imports: [
    NgForOf,
    Button,
    Tag,
    NgIf
  ],
  templateUrl: './payment-history.component.html',
  styleUrl: './payment-history.component.scss'
})
export class PaymentHistoryComponent {
  payments: PaymentRecord[] = [];

  constructor(private paymentRecordsService: PaymentRecordsService, private refundService: RefundService,
              private messageService: MessageService, private authService: AuthService) {
    if (this.authService.hasRole("ADMIN")){
      this.paymentRecordsService.getAllPayments().subscribe({
        next: data => {
          this.payments = data;
        },
        error: err => {
          console.error('Error fetching payment history:', err);
        }
      });
    } else {
      this.paymentRecordsService.getUserPaymentHistory().subscribe({
        next: data => {
          this.payments = data;
        },
        error: err => {
          console.error('Error fetching payment history:', err);
        }
      });
    }
  }

  async requestRefund(id: number){
    console.log("requested refund.");
    try {
      await this.refundService.requestPaymentRefund(id);
    } catch(error){
      console.error("Error requesting refund", error);
      this.messageService.add({severity: 'error', summary: 'Error', detail: 'Error requesting refund'});
    }
  }

  determineTagSeverity(status: string) {
    switch (status) {
      case 'Success':
        return 'success';
      case 'Pending':
        return 'warn';
      case 'Failed':
        return 'danger';
      case 'Refunded':
        return 'info';
      default:
        return 'info';
    }
  }
}
