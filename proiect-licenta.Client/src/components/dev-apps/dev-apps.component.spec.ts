import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevAppsComponent } from './dev-apps.component';

describe('DevAppsComponent', () => {
  let component: DevAppsComponent;
  let fixture: ComponentFixture<DevAppsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DevAppsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DevAppsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
