import {Component, EventEmitter, Input, Output} from '@angular/core';
import {Dimension, DimensionModalOptions} from "../../types/analytics";

@Component({
  selector: 'dimensions',
  templateUrl: './dimensions.component.html',
  styleUrls: [ "../../analytics.component.less" ]
})
export class DimensionsComponent {

  @Input() options: DimensionModalOptions = new DimensionModalOptions();
  @Output() onDeleteDimension = new EventEmitter<Dimension>();

  updateDimension(dimension: Dimension) {
    this.options.upsertDimension(dimension);
  }

  deleteDimension(dimension: Dimension) {
    this.onDeleteDimension.emit(dimension);
  }

}
