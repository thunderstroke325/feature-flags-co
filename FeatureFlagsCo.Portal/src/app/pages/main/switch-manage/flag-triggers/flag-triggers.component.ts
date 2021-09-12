import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';

@Component({
  selector: 'flag-triggers',
  templateUrl: './flag-triggers.component.html',
  styleUrls: ['./flag-triggers.component.less']
})
export class FlagTriggersComponent implements OnInit, OnDestroy {

  private destory$: Subject<void> = new Subject();
  featureFlagId: string;

  constructor(
    private route: ActivatedRoute,
  ) {}

  ngOnInit(): void {
    this.featureFlagId = this.route.snapshot.paramMap.get('id');
  }

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }
}
