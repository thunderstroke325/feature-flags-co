import { DataCard, Dimension, IDataItem } from "./analytics";

/**
 * 数据分组
 */
export const dataGrouping = (data: DataCard[] = [], envId: number) => {
    let groupingResult: sameTimeGroup[] = [];

    // 分相同时间段的组
    let sameTimeSlotList = {};

    data.forEach((card: DataCard) => {
        const startTime = card.startTime;
        const endTime = card.endTime;

        // 按 开始时间、结束时间、分析维度 分组
        const sortedDimensions = [...card.dimensions].sort();
        const sameTimeSlotKey = `${startTime}#${endTime}#${sortedDimensions}`;
        if(!sameTimeSlotList[sameTimeSlotKey]) {
            sameTimeSlotList[sameTimeSlotKey] = [];
        }
        sameTimeSlotList[sameTimeSlotKey].push(card);
    })

    // 相同时间段内对数据的计数方式进行分组
    Object.entries(sameTimeSlotList).forEach(([_, value]: [string, DataCard[]]) => {
        const items: IDataItem[] = value.reduce((acc: any, card: DataCard) => {
            acc = [...acc, ...card.items];
            return acc;
        }, []);

        // use first item to get startTime, endTime and selectedOptions
        const { startTime, endTime } = value[0];
        groupingResult.push({
            envId,
            startTime,
            endTime,
            items,
            dimensions: value[0].selectedDimensions()
        })
    })

    return groupingResult;
}

export interface groupItem {
    id: string;
    name: string;
    dataType: string;
    calculationType: number;
}

export interface sameTimeGroup {
    envId: number;
    startTime: Date;
    endTime: Date;
    items: IDataItem[];
    dimensions: Dimension[];
}
