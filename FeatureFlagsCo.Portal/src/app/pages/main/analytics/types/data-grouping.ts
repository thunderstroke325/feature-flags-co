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

        let sameCalculationTypeList = {};

        value.forEach((data: DataCard) => {
            if(data.items.length) {
                data.items.forEach((item: IDataItem) => {
                    const sameCalculationTypeKey = item.calculationType.toString();

                    if(!sameCalculationTypeList[sameCalculationTypeKey]) {
                        sameCalculationTypeList[sameCalculationTypeKey] = [];
                    }

                    sameCalculationTypeList[sameCalculationTypeKey].push({
                        id: item.id,
                        name: item.name,
                        dataType: item.dataSource.dataType
                    })
                })
            }
        })

        Object.entries(sameCalculationTypeList).forEach(([key, value]: [string, groupItem[]]) => {
            groupingResult.push({
                envId,
                startTime,
                endTime,
                calculationType: parseInt(key),
                items: value
            })
        })
    })

    return groupingResult;
}

interface groupItem {
    id: string;
    name: string;
    dataType: string;
}

export interface sameTimeGroup {
    envId: number;
    startTime: string;
    endTime: string;
    calculationType: number;
    items: groupItem[];
}