import { Injectable, Inject } from '@angular/core';
import { FFCJsClient } from 'ffc-js-client-sdk/esm';
import { IFFCUser, IOption } from 'ffc-js-client-sdk/esm/types';
import { environment } from '../../environments/environment';


@Injectable({
  providedIn: 'root'
})
export class FfcService {

  private client = FFCJsClient;
  constructor() {
  }

  variation (featureFlagKey: string, defaultResult?: string, standaloneValue?: string): string {
    return environment.name === 'Standalone' ? standaloneValue : this.client.variation(featureFlagKey, defaultResult);
  }

  initialize (environmentSecret: string, user?: IFFCUser, option?: IOption): void {
    environment.name !== 'Standalone' && this.client.initialize(environmentSecret, user, option);
  }
}
