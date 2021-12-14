import { uuidv4 } from 'src/app/utils';

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
    isLoading?: boolean, // only for UI
    isEditing?: boolean,
    isTooltip?: boolean;
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
    isLoading?: boolean; // only for UI
    isEditing?: boolean;
    isTooltip?: boolean;
    itemsCount: number;

    constructor(data?: IDataCard) {
        if (data) {
            this.id = data.id;
            this.name = data.name;
            this.startTime = !!data.startTime ? new Date(data.startTime) : null;
            this.endTime = !!data.startTime ? new Date(data.endTime) : null;
            this.isLoading = data.isLoading;
            this.isEditing = data.isEditing;
            this.isTooltip = data.isTooltip;
            this.items = [...data.items];
            this.itemsCount = data.itemsCount;
        } else {
            this.id = uuidv4();
            this.name = null;
            this.isLoading = false;
            this.isEditing = true;
            this.items = [];
            // new Array(6).fill({}).map((_i, index) => ({
            //   id: uuidv4(),
            //   name: null,
            //   value: null,
            //   unit: null,
            //   color: null,
            //   calculationType: CalculationType.Count
            // }))
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
                item.calculationType = data.calculationType,
                item.isSetupDataSource = data.isSetupDataSource
            }
        })
    }
}

// 数据源
export interface dataSource {
    id: string;
    name: string;
    dataType: string;
    keyName: string;
}

// 更新看板数据参数
export interface updataReportParam {
    analyticBoardId: string;
    envId: number;
    id: string;
    name: string;
    startTime: Date;
    endTime: Date;
    items: IDataItem[]
}