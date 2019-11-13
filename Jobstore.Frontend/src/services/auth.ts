import axios from 'axios'

import { LoginModel } from '../models'
import LoginResponse from '../models/apiResponses/LoginResponse'
import setAuthToken from '../untils/setAuthToken';

const authService = {
    login,
    logout,
    getUserId,
    getExpirationDate
};

async function login(model: LoginModel): Promise<LoginResponse> {
    try {
        const response = await axios.post<LoginResponse>('/auth/login', model);
        const expirationDate = new Date(new Date().getTime() + response.data.expires_in * 1000);
        localStorage.setItem('token', response.data.auth_token);
        localStorage.setItem('expirationDate', expirationDate.toString());
        localStorage.setItem('userId', response.data.id);
        setAuthToken(response.data.auth_token);
        return response.data;
    }
    catch (error) {
        throw (error.response && error.response.data) || 'Internal Server Error';
    }
}

function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('expirationDate');
    localStorage.removeItem('userId');
    setAuthToken(null);
}

function getUserId(): string | null {
    const token = localStorage.getItem('token') as string;
    if (token != null) {
        setAuthToken(token);
        return  localStorage.getItem('userId');
    }
    return null;
}

function getExpirationDate(): Date | null {
    const expirationDate = localStorage.getItem('expirationDate');
    if (expirationDate != null) {
        return  new Date(expirationDate);
    }
    return null;
}

export default authService;