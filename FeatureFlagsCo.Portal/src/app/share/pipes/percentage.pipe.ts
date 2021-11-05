import { Pipe, PipeTransform } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";

@Pipe({ name: "percentage" })
export class PercentagePipe implements PipeTransform {
  constructor(private sanitizer: DomSanitizer) {}

  transform(value) {
    if (value === -1 || value === '--') {
      return '--'
    } else {
      return (value * 100).toFixed(1) + '%'
    }
  }
}
