import http from 'k6/http';
import { sleep } from 'k6';

const API_URL = 'https://localhost:7073/api/polly/200?type=cache';

export const options = {
    stages: [
        { duration: '5s', target: 3 },
        { duration: '34s', target: 5 },
        { duration: '5s', target: 21 },
        { duration: '34s', target: 21 },
        { duration: '5s', target: 144 },
        { duration: '34s', target: 89 },
        { duration: '5s', target: 233 },
        { duration: '34s', target: 377 },
        { duration: '5s', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'],
    },
};

export default function () {
    http.get(API_URL);
    sleep(1);
}