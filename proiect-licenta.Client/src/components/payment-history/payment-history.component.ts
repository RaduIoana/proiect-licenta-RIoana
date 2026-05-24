import { Component } from '@angular/core';
import {NgForOf, NgIf} from "@angular/common";
import {Router} from '@angular/router';
import {PaymentRecord, PaymentRecordsService} from '../../services/payment-records.service';
import {Button} from 'primeng/button';
import {MessageService} from 'primeng/api';
import {RefundService} from '../../services/refund.service';
import {Tag} from 'primeng/tag';

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

  constructor(private router: Router, private paymentRecordsService: PaymentRecordsService,
              private refundService: RefundService, private messageService: MessageService) {
    this.paymentRecordsService.getUserPaymentHistory().subscribe({
      next: (data) => {
        this.payments = data.map(obj => ({...obj, canRefund: this.determineRefund(obj)}));
      },
      error: err => {
        console.error('Error fetching payment history:', err);
      }
    });
  }

  determineRefund(paymentRecord: PaymentRecord): boolean {
    if (paymentRecord.paymentStatus == "success"
      && paymentRecord.paymentType != 0
      && this.within48Hrs(paymentRecord.paymentDT)
    )
      return true;
    return false;
  }

  within48Hrs(paymentDT: string): boolean {
    const interval = Math.abs(new Date().getTime() - new Date(paymentDT).getTime());
    return interval <= 48 * 60 * 60 * 1000;
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
