import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { FlagTriggersRoutingModule } from './flag-triggers-routing.module';
import { ComponentsModule } from '../components/components.module';
import { FlagTriggersComponent } from './flag-triggers.component';

@NgModule({
  declarations: [FlagTriggersComponent],
  imports: [
    CommonModule,

    ComponentsModule,
    FlagTriggersRoutingModule
  ]
})
export class FlagTriggersModule { }
