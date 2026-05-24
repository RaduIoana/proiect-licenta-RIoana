import { TestBed } from '@angular/core/testing';

import { PaymentRecordsService } from './payment-records.service';

describe('PaymentRecordsService', () => {
  let service: PaymentRecordsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PaymentRecordsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
