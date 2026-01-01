export interface IRegisterRequest {
  email: string;
  name: string;
  surname: string;
  password: string;
}

export interface ILoginRequest {
  email: string;
  password: string;
}

export interface ILoginResponse {
  accessToken: string;
  refreshToken: string;
}
