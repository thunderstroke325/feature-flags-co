import {AfterViewInit, Component, Input} from '@angular/core';
import {Chart} from "@antv/g2";
import {AxisConfig, ChartConfig, defaultTooltipItemTplPlaceholder} from "./g2-line-chart";
import {MacaronColors} from "../g2-chart";

@Component({
  selector: 'g2-line-chart',
  template: `
    <div id="line-chart-container-{{this.containerId ? this.containerId : ''}}" [style]="containerStyle">
    </div>
  `
})
export class G2LineChartComponent implements AfterViewInit {
  @Input()
  containerId: string = '';
  @Input()
  containerStyle: string = "height: 400px; width: 100%;";
  @Input()
  chartConfig: ChartConfig;

  chart: Chart;

  ngAfterViewInit(): void {
    if (!this.chartConfig) {
      console.error('failed to render chart, invalid chart config...');
      return;
    }

    this.renderChart();
  }

  private renderChart() {
    this.chart = new Chart({
      container: `line-chart-container-${this.containerId}`,
      autoFit: true,
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
      itemTpl: toolTip && toolTip.tplFormatter
        ? toolTip.tplFormatter(defaultTooltipItemTplPlaceholder)
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

  private configChartAxis({field, formatter, name, position, scale}: AxisConfig): void {
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
