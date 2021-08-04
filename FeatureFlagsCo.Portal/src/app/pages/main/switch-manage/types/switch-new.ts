export interface IUserType {
    id: string;
    keyId?: string;
    name?: string;
    objectType?: string;
    environmentId?: number;
    featureFlagUserPercentageRecords?: any[];
    email?: string;
    customizedProperties?: [{name: string, value: string}];
    country?: string;
}

export interface IFfParams {
    id: string;
    name: string;
    keyName: string;
    environmentId: number;
    creatorUserId: string;
    status: 'Enabled' | 'Disabled';
    defaultRuleValue: boolean | string;
    percentageRolloutForTrue: number;
    percentageRolloutForFalse: number;
    percentageRolloutBasedProperty: boolean;
    valueWhenDisabled: boolean;
    lastUpdatedTime: string;
}

export interface IFfpParams {
    prerequisiteFeatureFlagId: string;
    variationValue: boolean;
}

export interface IFftiuParams {
    id: string;
    name: string;
    keyId: string;
    email: string;
}

export interface IJsonContent {
    property: string;
    operation: string;
    value: string;

    multipleValue?: string[];
    type?: string;
}

export interface IFftuwmtrParams {
    ruleId: string;
    ruleName: string;
    ruleJsonContent: IJsonContent[];
    variationRuleValue: boolean | string;
    percentageRolloutForTrue: number;
    percentageRolloutForFalse: number;
    percentageRolloutBasedProperty: string;
}

export class CSwitchParams {
    private id: string;
    private environmentId: number;
    private objectType: string;

    private ff: IFfParams;
    private ffp: IFfpParams[];
    private fftiuForFalse: IFftiuParams[];
    private fftiuForTrue: IFftiuParams[];
    private fftuwmtr: IFftuwmtrParams[];

    constructor(data: CSwitchParams) {

        this.id = data.id;
        this.environmentId = data.environmentId;
        this.objectType = data.objectType;

        this.ff = data.ff;
        this.ffp = data.ffp;
        this.fftiuForFalse = data.fftiuForFalse;
        this.fftiuForTrue = data.fftiuForTrue;
        this.fftuwmtr = data.fftuwmtr;

        this.initFFNullString();
        this.initFFTuwmtrNullString();
    }

    // 获取默认返回值
    // 默认选项
    public getFFDefaultRuleValue() {
        return this.ff.defaultRuleValue;
    }

    // true 与 false 的数值
    public getPercentageValue(type: 'true' | 'false') {
        if(type === 'true') {
            return this.ff.percentageRolloutForTrue;
        } else {
            return this.ff.percentageRolloutForFalse;
        }
    }

    // 获取默认返回值
    public getFFBasedProperty(): boolean {
        return this.ff.valueWhenDisabled;
    }

    // 设置默认返回值
    public setFFBasedProperty(value: boolean) {
        this.ff.valueWhenDisabled = value;
    }

    // 设置默认返回值
    public setFFConfig(value: { serve: boolean | string, F: number, T: number }) {
        this.ff.defaultRuleValue = value.serve;
        let trueValue = value.serve !== 'null' ? null : value.T;
        this.ff.percentageRolloutForTrue = Number((trueValue / 100).toFixed(2));
        this.ff.percentageRolloutForFalse = trueValue !== null ? 1 - this.ff.percentageRolloutForTrue : null;
    }

    // 设置 ff 字段的 defaultRuleValue 属性值
    private initFFNullString() {
        let result = this.needNullString(this.ff.defaultRuleValue as boolean, this.ff.percentageRolloutForTrue, this.ff.percentageRolloutForFalse);
        result && (this.ff.defaultRuleValue = 'null');
    }

    // 设置 fftuwmtr 字段的 defaultRuleValue 属性值
    private initFFTuwmtrNullString() {
        this.fftuwmtr.forEach((fft: IFftuwmtrParams) => {
            let result = this.needNullString(fft.variationRuleValue as boolean, fft.percentageRolloutForTrue, fft.percentageRolloutForFalse);
            result && (fft.variationRuleValue = 'null');
        })
    }

    // 判断是否需要将 null 改为 字符串 ‘null’
    private needNullString(value1: boolean, value2: number, value3: number): boolean {
        return (value1 == null && value2 != null && value3 != null);
    }

    // 设置当前开关状态
    public setFeatureStatus(status: 'Enabled' | 'Disabled') {
        this.ff.status = status;
    }

    // 获取当前开关状态
    public getFeatureStatus(): 'Enabled' | 'Disabled' {
        return this.ff.status;
    }

    // 设置上游开关列表
    public setUpperFeatures(data: IFfpParams[]) {
        this.ffp = [...data];
    }

    // 获取上游开关列表
    public getUpperFeatures(): IFfpParams[] {
        return this.ffp;
    }

    // 获取目标用户
    public getTargetUsers(type: 'true' | 'false') {
        if(type === 'true') {
            return this.fftiuForTrue;
        } else {
            return this.fftiuForFalse;
        }
    }

    // 设置目标用户
    public setTargetUsers(type: 'true' | 'false', data: IUserType[]) {
        let lists: IFftiuParams[] = [];
        data.forEach((item: IUserType) => {
            let list: IFftiuParams = {
                id: item.id,
                name: item.name,
                keyId: item.keyId,
                email: item.email
            }
            lists.push(list);
        })
        if(type === 'true') {
            this.fftiuForTrue = [...lists];
        } else {
            this.fftiuForFalse = [...lists];
        }
    }

    // 获取匹配规则
    public getFftuwmtr(): IFftuwmtrParams[] {
        return this.fftuwmtr;
    }

    // 删除匹配规则
    public deleteFftuwmtr(index: number) {
        this.fftuwmtr.splice(index, 1);
    }

    // 添加匹配规则
    public addFftuwmtr() {
        this.fftuwmtr.push({
            ruleId: '',
            ruleName: '',
            ruleJsonContent: [],
            variationRuleValue: null,
            percentageRolloutForTrue: null,
            percentageRolloutForFalse: null,
            percentageRolloutBasedProperty: null
        })
    }

    // 设置规则 serve
    public setConditionServe(value: { serve: boolean | string, F: number, T: number }, index: number) {
        this.fftuwmtr[index].variationRuleValue = value.serve;
        let trueValue = value.serve !== 'null' ? null : value.T;
        this.fftuwmtr[index].percentageRolloutForTrue = Number((trueValue / 100).toFixed(2));
        this.fftuwmtr[index].percentageRolloutForFalse = trueValue !== null ? 1 - this.fftuwmtr[index].percentageRolloutForTrue : null;
    }

    // 设置字段信息
    public setConditionConfig(value: IJsonContent[], index: number) {
        this.fftuwmtr[index].ruleJsonContent = [...value];
    }

    // 处理提交数据
    public onSortoutSubmitData() {
        let ffDataFilters: IFfpParams[] = this.ffp.filter((item: IFfpParams) => item.variationValue !== null && item.prerequisiteFeatureFlagId !== null);
        this.ffp = [...ffDataFilters];

        // this.ff.defaultRuleValue = this.ff.defaultRuleValue === "null" ? null : this.ff.defaultRuleValue;

        this.fftuwmtr.forEach((item: IFftuwmtrParams) => {
            item.variationRuleValue = item.variationRuleValue === 'null' ? null : item.variationRuleValue;

            item.ruleJsonContent.forEach((rule: IJsonContent) => {
                if(rule.type === 'multi') {
                    rule.value = JSON.stringify(rule.multipleValue);
                }
                if(rule.type === 'number') {
                    rule.value = rule.value.toString();
                }
            })
        })
    }

    // 获取开关详情
    public getSwicthDetail(): IFfParams {
        return this.ff;
    }
}
