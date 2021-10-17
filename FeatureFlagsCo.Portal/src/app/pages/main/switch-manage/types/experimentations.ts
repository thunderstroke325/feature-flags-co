export enum EventType {
  Custom = 1,
  PageView = 2,
  Click = 3
}

export enum CustomEventTrackOption {
  Undefined = 0,
  Conversion = 1,
  Numeric = 2
}

export enum CustomEventSuccessCriteria {
  Undefined = 0,
  Lower = 1, // lower than baseline
  Higher = 2 // higher than baseline
}

export enum ExperimentStatus {
  NotStarted = 1,
  NotRecording = 2,
  Recording = 3
}

export interface IMetric {
  id?: string,
  name: string,
  envId: number,
  description: string,
  maintainerUserId: string,
  eventName: string,
  eventType: EventType,
  customEventTrackOption?: CustomEventTrackOption,
  customEventUnit?: string,
  customEventSuccessCriteria?: CustomEventSuccessCriteria
}

export interface IExperiment {
  id?: string,
  envId: number,
  featureFlagId: string,
  featureFlagName?: string,
  metricId: string,
  metric?: IMetric,
  baselineVariation: string,
  status?: ExperimentStatus,
  variations: string[],
  iterations?: IExperimentIteration[],
  selectedIteration?: IExperimentIteration,
  isLoading?: boolean
}

export interface IExperimentIteration {
  id: string,
  startTime: Date,
  endTime: Date,
  updatedAt?: Date,
  updatedAtStr?: string,
  results: IExperimentIterationResult[],
  dateTimeInterval?: string,
  numericConfidenceIntervalBoundary?: number[] // [min, max, max - min]
}

export interface IExperimentIterationResult {
  changeToBaseline: number, // float
  conversion: number, // long
  conversionRate: number, // float
  isBaseline: boolean,
  isInvalid: boolean,
  IsWinner: boolean,
  pValue: number, // float
  uniqueUsers: number, // long
  variation: string,
  confidenceInterval: number[] // float[]
}
