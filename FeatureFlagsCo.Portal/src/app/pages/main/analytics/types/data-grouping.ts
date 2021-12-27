import { DataCard, Dimension, IDataItem } from "./analytics";
import moment from "moment";

/**
 * 数据分组
 */
export const dataGrouping = (data: DataCard[] = [], envId: number) => {
    let groupingResult: sameTimeGroup[] = [];

    // 按 开始时间、结束时间、分析维度 分组
    let slotList = {};

    data.forEach((card: DataCard) => {
        // use hour time (time with 0 mins, 0 secs, and 0 ms) to group cards
        // also see AnalyticsService.computeResult method
        const startTime = moment(card.startTime).startOf('hour').format('YYYY-MM-DD HH:mm:ss');
        const endTime = moment(card.endTime).startOf('hour').format('YYYY-MM-DD HH:mm:ss');

        const sortedDimensions = [...card.dimensions].sort();
        const slotKey = `${startTime}#${endTime}#${sortedDimensions}`;
        if(!slotList[slotKey]) {
            slotList[slotKey] = [];
        }
        slotList[slotKey].push(card);
    })

    // 相同时间段内对数据的计数方式进行分组
    Object.entries(slotList).forEach(([_, value]: [string, DataCard[]]) => {
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
