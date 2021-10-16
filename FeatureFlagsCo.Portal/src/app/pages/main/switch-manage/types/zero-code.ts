import { IVariationOption } from "./switch-new";

export interface IZeroCode {
  envId: number,
  envSecret: string,
  isActive: boolean,
  featureFlagId: string,
  featureFlagKey: string,
  items: ICssSelectorItem[]
}

export interface ICssSelectorItem {
  id: string,
  cssSelector: string,
  description: string,
  variationOption: IVariationOption,
  url: string
}
