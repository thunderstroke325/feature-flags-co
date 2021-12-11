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
        const startTime = moment(card.startTime).format("YYYY-MM-DD");
        const endTime = moment(card.endTime).format("YYYY-MM-DD");
        const sameTimeSlotKey = `${startTime}#${endTime}`;
        
        if(!sameTimeSlotList[sameTimeSlotKey]) {
            sameTimeSlotList[sameTimeSlotKey] = [];
        }
        sameTimeSlotList[sameTimeSlotKey].push(card);
    })

    // 相同时间段内对数据的计数方式进行分组
    Object.entries(sameTimeSlotList).forEach(([key, value]: [string, DataCard[]]) => {

        const [startTime, endTime] = key.split("#");

        let items: groupItem[] = [];

        value.forEach((data: DataCard) => {
            if(data.items.length) {
                data.items.forEach((item: IDataItem) => {
                    items.push({
                        id: item.id,
                        name: item.name,
                        dataType: item.dataSource.dataType,
                        calculationType: item.calculationType
                    })
                })
            }
        })

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
    startTime: string;
    endTime: string;
    items: groupItem[];
}