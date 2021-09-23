export interface IAuthProps {
    email: string
    phoneNumber: string
}

export interface IAccount {
  id: number,
  organizationName: string
}

export interface IAccountProjectEnv {
  account: IAccount,
  projectEnv: IProjectEnv
}

export interface IAccountUser {
  userId: string,
  email: string,
  role: string,
  initialPassword: string
}

export interface IEnvironment {
  id: number,
  projectId: number,
  name: string,
  description: string,
  secret: string,
  mobileSecret: string
}

export interface IProject {
  id: number,
  name: string,
  environments: IEnvironment[]
}

export interface IProjectEnv {
  projectId: number,
  projectName: string,
  envId: number,
  envName: string,
  envSecret: string
}

export enum EnvKeyNameEnum {
  Secret = "Secret",
  MobileSecret = "MobileSecret"
}

export interface IEnvKey {
  keyName: EnvKeyNameEnum,
  keyValue?: string
}
