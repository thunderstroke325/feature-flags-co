import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'sdk-java-integration',
  templateUrl: './sdk-java.component.html',
  styleUrls: ['./sdk-java.component.less']
})
export class SDKJavaComponent implements OnInit {

  

  constructor() {}

  @Input() envSecret: string;
  @Input() keyName: string;

  ngOnInit(): void {
  }
}
