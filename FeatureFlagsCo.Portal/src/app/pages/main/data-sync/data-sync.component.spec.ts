import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DataSyncComponent } from './data-sync.component';

describe('DataSyncComponent', () => {
  let component: DataSyncComponent;
  let fixture: ComponentFixture<DataSyncComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DataSyncComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DataSyncComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
