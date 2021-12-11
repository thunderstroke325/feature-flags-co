import { dataSource } from "./analytics";

export const hasSameKeyName = (dataSourceList: dataSource[], dataSource: dataSource) => {
    const index = dataSourceList.findIndex(item => item.keyName && item.keyName === dataSource.keyName);
    return index !== -1;
}