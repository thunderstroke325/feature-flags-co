import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { environment } from 'src/environments/environment';
import packageInfo from '../../package.json';
import { FfcService } from './services/ffc.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.less']
})
export class AppComponent {
  isCollapsed = false;

  constructor(
    ffcService: FfcService
  ) {
    console.log(`Current Version: ${packageInfo.version}`);
    (()=>{
      ffcService.initialize({
        secret: environment.projectEnvKey,
        anonymous: true
      });
    })();
  }
}
