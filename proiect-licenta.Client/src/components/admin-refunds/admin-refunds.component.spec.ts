import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminRefundsComponent } from './admin-refunds.component';

describe('AdminRefundsComponent', () => {
  let component: AdminRefundsComponent;
  let fixture: ComponentFixture<AdminRefundsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminRefundsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminRefundsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
