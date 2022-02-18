import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'sdk-js-integration',
  templateUrl: './sdk-js.component.html',
  styleUrls: ['./sdk-js.component.less']
})
export class SDKJSComponent implements OnInit {

  jsModeNpm = 'npm';
  jsModeScript = 'script';
  jsMode = this.jsModeScript;

  jsCodeNpm = '';
  jsCodeScript = `
    <script src="https://assets.feature-flags.co/sdks/ffc-sdk.js"></script>`;
  jsCodeCommon = '';
  jsNpmInstall = `
    npm install ffc-js-client-side-sdk --save`;

  constructor() {}

  @Input() envSecret: string;
  @Input() keyName: string;

  ngOnInit(): void {
    this.jsCodeCommon = `
    // 初始化 SDK，已有用户信息时
    Ffc.init({
      secret: '${this.envSecret}',
      user: {
        id: 'example-user', // your user unique identifier
        userName: 'example-user',
        email:'example-user@example.com'
      }
    });

    // 初始化 SDK, 还没有用户时，传入 anonynous 参数，SDK 将生成匿名用户
    Ffc.init({
      secret: '${this.envSecret}',
      anonymous: true
    });

    // 登录后通过 identify 方法传入用户信息
    Ffc.identify({
      id: 'example-user', // your user unique identifier
      userName: 'example-user',
      email:'example-user@example.com'
    });

    // 调用开关
    // ffcscan ignore 我们这里需要这行来告诉 code reference 工具忽略此行，复制后请去掉这行注释。工具在这里：https://github.com/feature-flags-co/ffc-code-refs-core
    const myVar = Ffc.variation('${this.keyName}', 'default value');
    
    // 当开关值发生变化时获取通知
    Ffc.on('ff_update:${this.keyName}', (change) => {
      console.log(change['newValue']); // 请将此行替换成自己的代码
    });
    `
  
    this.jsCodeNpm = `
    import Ffc from 'ffc-js-client-side-sdk';
    ${this.jsCodeCommon}
    `;
  }
}
