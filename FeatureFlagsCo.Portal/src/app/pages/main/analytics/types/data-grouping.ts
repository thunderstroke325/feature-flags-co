import { DataCard, IDataItem } from "./analytics";
import * as moment from 'moment';

/**
 * 数据分组
 *  按相同时间段和相同计数方式分组
 */
export const dataGrouping = (data: DataCard[] = [], envId: number) => {
    let groupingResult: sameTimeGroup[] = [];

    // 分相同时间段的组
    let sameTimeSlotList = {};

    data.forEach((card: DataCard) => {
        const startTime = card.startTime;
        const endTime = card.endTime;

        const sameTimeSlotKey = `${startTime}#${endTime}`;
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

        // use first item to get startTime and endTime
        const { startTime, endTime } = value[0];
        groupingResult.push({
            envId,
            startTime, 
            endTime,
            items
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
}