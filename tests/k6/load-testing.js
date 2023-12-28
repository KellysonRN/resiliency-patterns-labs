import http from 'k6/http';
import { sleep } from 'k6';

const API_URL = 'https://localhost:7073/api/polly/200';

export const options = {
    stages: [
        { duration: '5s', target: 5 },
        { duration: '21s', target: 21 },
        { duration: '8s', target: 8 },
        { duration: '13s', target: 13 },
        { duration: '5s', target: 5 },
        { duration: '34s', target: 34 },
        { duration: '3s', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'],
    },
};


export default function () {
    http.get(API_URL);
    sleep(1);
}