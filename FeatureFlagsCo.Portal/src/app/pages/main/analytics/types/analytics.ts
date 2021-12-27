import { uuidv4 } from 'src/app/utils';
import { NzSelectOptionInterface } from "ng-zorro-antd/select";

export enum CalculationType {
    Count = 1,
    Sum = 2,
    Average = 3
}

export interface IDataCard {
    id: string,
    name: string,
    startTime: Date,
    endTime: Date,
    items: IDataItem[],
    itemsCount: number,
    dimensions: string[];

    // only for UI
    isLoading?: boolean,
    isEditing?: boolean,
    isTooltip?: boolean;
    dimensionOptions: NzSelectOptionInterface[];
}

export interface IDataItem {
    id: string,
    name: string,
    value: number,
    dataSource: dataSource,
    unit: string,
    color: string,
    calculationType: CalculationType,
    isLoading?: boolean;
    isSetupDataSource?: boolean;
}

export class DataCard {
    id: string;
    name: string;
    startTime: Date;
    endTime: Date;
    items: IDataItem[];
    dimensions: string[] = [];
    itemsCount: number;

    // only for UI
    isLoading?: boolean;
    isEditing?: boolean;
    isTooltip?: boolean;
    dimensionOptions: NzSelectOptionInterface[] = [];

    constructor(data?: IDataCard) {
        if (data) {
            this.id = data.id;
            this.name = data.name;
            this.startTime = !!data.startTime ? new Date(data.startTime) : null;
            this.endTime = !!data.endTime ? new Date(data.endTime) : null;
            this.isLoading = data.isLoading;
            this.isEditing = data.isEditing;
            this.isTooltip = data.isTooltip;
            this.items = [...data.items];
            this.itemsCount = data.itemsCount;

            // dimension select options
            this.dimensions = [...data.dimensions];
            this.dimensionOptions = [...data.dimensionOptions];
            if (this.dimensions.length) {
              this.onSelectDimension();
            }
        } else {
            this.id = uuidv4();
            this.name = null;
            this.isLoading = false;
            this.isEditing = true;
            this.items = [];
        }
    }

    disabledStartDate = (startTime: Date): boolean => {
        if (!startTime || !this.endTime) {
            return false;
        }
        return startTime.getTime() > this.endTime.getTime();
    }

    disabledEndDate = (endTime: Date): boolean => {
        if (!endTime || !this.startTime) {
            return false;
        }
        return endTime.getTime() <= this.startTime.getTime();
    }

    clearStartDate = (): void => {
        this.startTime = null;
    }

    clearEndDate = (): void => {
        this.endTime = null;
    }

    // 更新 item
    public updateItem(data: IDataItem) {
        this.items.forEach(item => {
            if(data.id === item.id) {
                item.dataSource = data.dataSource;
                item.unit = data.unit;
                item.color = data.color;
                item.calculationType = data.calculationType;
                item.isSetupDataSource = data.isSetupDataSource
            }
        })
    }

    onSelectDimension() {
      const newOptions = [...this.dimensionOptions];
      if (!this.dimensions.length) {
        newOptions.forEach(item => {
          item.disabled = false;
        });

        this.dimensionOptions = newOptions;
        return;
      }

      const selectedOptions = this.dimensionOptions.filter(item => this.dimensions.includes(item.value));
      newOptions.forEach(option => {
        if (selectedOptions.includes(option)) {
          return;
        }

        option.disabled = selectedOptions
          .map(item => item.groupLabel)
          .includes(option.groupLabel);
      });

      this.dimensionOptions = newOptions;
    }

    selectedDimensions(): Dimension[] {
      const selected: Dimension[] = [];

      this.dimensions.forEach(dimension => {
        const option = this.dimensionOptions.find(option => option.value == dimension);
        if (option) {
          selected.push({
            id: option.value,
            key: option.groupLabel as string,
            value: option.label as string
          });
        }
      })

      return selected;
    }

    prettyPrintSelectedDimensions() {
      return this.selectedDimensions()
        .map(option => `${option.key}:${option.value}`)
        .join(', ');
    }
}

// 数据源
export interface dataSource {
    id: string;
    name: string;
    dataType: string;
    keyName: string;
}

// 维度
export interface Dimension {
  id: string;
  key: string;
  value: string;
}

export interface UpsertDimensionVm {
  analyticBoardId: string,
  envID: number,
  id: string;
  key: string;
  value: string;
}

// 维度模态框配置
export class DimensionModalOptions {
  visible: boolean;
  mode: "show-list" | "upsert-form";
  dimensions: Dimension[];
  currentDimension: Dimension;

  constructor() {
    this.visible = false;
    this.mode = 'show-list';
    this.dimensions = [];
    this.currentDimension = null;
  }

  open() {
    this.visible = true;
    this.mode = 'show-list';
  }

  handleCancel() {
    switch (this.mode) {
      // close dialog
      case "show-list":
        this.visible = !this.visible;
        break;

      // back to show-list
      case "upsert-form":
        this.mode = 'show-list';
    }
  }

  handleOk(dimension?: Dimension) {
    if (dimension) {
      // update current dimension
      this.currentDimension = dimension;
    }

    switch (this.mode) {
      // go to add form
      case "show-list":
        this.mode = 'upsert-form';
        break;

      // after upsert, back to show-list
      case "upsert-form":
        this.mode = 'show-list';
        break;
    }
  }

  upsertDimension(dimension?: Dimension) {
    if (dimension) {
      this.currentDimension = dimension;
    }

    let theIndex = this.dimensions.findIndex(dimension => dimension.id === this.currentDimension.id);
    if (theIndex !== -1) {
      // update
      this.dimensions.splice(theIndex, 1, this.currentDimension);
      this.dimensions = Object.assign([], this.dimensions);
    } else {
      // insert
      this.dimensions = [...this.dimensions, this.currentDimension];
    }

    this.handleOk();
  }

  deleteDimension(toDelete: Dimension) {
    const theIndex = this.dimensions.findIndex(dimension => dimension.id == toDelete.id);
    if (theIndex !== -1) {
      this.dimensions = this.dimensions.filter(x => x.id !== toDelete.id);
    }
  }
}

// 更新看板数据参数
export interface updataReportParam {
    analyticBoardId: string;
    envId: number;
    id: string;
    name: string;
    startTime: Date;
    endTime: Date;
    items: IDataItem[],
    dimensions: string[],
}
