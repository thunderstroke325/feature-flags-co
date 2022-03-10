import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime } from 'rxjs/operators';
import { SwitchService } from 'src/app/services/switch.service';

@Component({
  selector: 'segment-manage',
  template: `<router-outlet></router-outlet>`
})
export class SegmentsComponent {

  private destory$: Subject<void> = new Subject();

  constructor(
    private switchServe: SwitchService
  ) {

    // this.workspaceService.workspaceHasChanged$
    //   .pipe(
    //     takeUntil(this.destory$),
    //     debounceTime(200),
    //   )
    //   .subscribe(
    //     res => {
    //       this.switchServe.workspaceID = this.workspaceService.getCurrentWorkspace().workspaceId;
    //     }
    // );
  }
}
