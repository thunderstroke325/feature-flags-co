import {AfterViewInit, Component, Input} from '@angular/core';
import {Chart} from "@antv/g2";
import {AxisConfig, ChartConfig, defaultTooltipItemTplPlaceholder} from "./g2-line-chart";
import {MacaronColors} from "../g2-chart";

@Component({
  selector: 'g2-line-chart',
  template: `
    <div id="line-chart-container-{{this.chartConfig.containerId ? this.chartConfig.containerId : ''}}">
    </div>
  `
})
export class G2LineChartComponent implements AfterViewInit {
  @Input()
  chartConfig: ChartConfig;

  chart: Chart;

  ngAfterViewInit(): void {
    if (!this.chartConfig) {
      console.error('failed to render chart, invalid chart config...');
      return;
    }

    this.chart = new Chart({
      container: `line-chart-container-${this.chartConfig.containerId}`,
      autoFit: true,
      height: this.chartConfig.height,
      padding: this.chartConfig.padding
    });

    this.chart.data(this.chartConfig.source);

    const xAxis = this.chartConfig.xAxis;
    const yAxis = this.chartConfig.yAxis;

    this.configChartAxis(xAxis);
    this.configChartAxis(yAxis);

    const toolTip = this.chartConfig.toolTip;
    this.chart.tooltip({
      showCrosshairs: true,
      shared: true,
      itemTpl: toolTip && toolTip.valueTplFormatter
        ? toolTip.valueTplFormatter(defaultTooltipItemTplPlaceholder)
        : defaultTooltipItemTplPlaceholder
    });

    const line = this.chart
      .line()
      .position(`${xAxis.field}*${yAxis.field}`);

    const point = this.chart
      .point()
      .position(`${xAxis.field}*${yAxis.field}`)

    const dataGroupBy = this.chartConfig.dataGroupBy;
    if (dataGroupBy) {
      line.color(this.chartConfig.dataGroupBy, MacaronColors);
      point.color(this.chartConfig.dataGroupBy, MacaronColors);
    }

    this.chart.render();
  }

  configChartAxis({field, formatter, name, position, scale}: AxisConfig): void {
    this.chart.axis(field, {
      title: {
        text: name,
        position: position
      },
      label: {
        formatter: formatter
      }
    });

    if (scale) {
      this.chart.scale(field, scale);
    }
  }
}
