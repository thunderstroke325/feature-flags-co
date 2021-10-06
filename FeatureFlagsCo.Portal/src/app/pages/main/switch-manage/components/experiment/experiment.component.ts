import { Component, Input, OnInit } from '@angular/core';
import { NzMessageService } from 'ng-zorro-antd/message';
import { IExperiment, IExperimentIteration } from '../../types/experimentations';

@Component({
  selector: 'app-experiment',
  templateUrl: './experiment.component.html',
  styleUrls: ['./experiment.component.less']
})
export class ExperimentComponent implements OnInit {

  isLoading: boolean = false;

  @Input() experiment: IExperiment;
  currentIteration: IExperimentIteration;
  constructor(
    private message: NzMessageService) {
  }

  ngOnInit(): void {
    // by default set the last iteration as the current one
    this.currentIteration = this.experiment.iterations[this.experiment.iterations.length - 1];

  }
}
