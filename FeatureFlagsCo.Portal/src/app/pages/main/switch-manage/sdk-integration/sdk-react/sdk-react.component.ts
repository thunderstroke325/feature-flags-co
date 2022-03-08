import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'sdk-react-integration',
  templateUrl: './sdk-react.component.html',
  styleUrls: ['./sdk-react.component.less']
})
export class SDKReactComponent implements OnInit {
  constructor() {}

  @Input() envSecret: string;
  @Input() keyName: string;

  ngOnInit(): void {
  }
}
