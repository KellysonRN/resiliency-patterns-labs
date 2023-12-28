import http from 'k6/http';
import { sleep } from 'k6';

const API_URL = 'https://localhost:7073/api/polly/200';

export const options = {
    stages: [        
        { duration: '10m', target: 16},
        { duration: '1h', target: 16 },
        { duration: '5m', target: 5 },
        { duration: '1m', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<600'],
    },
};

export default function () {
    http.get(API_URL);
    sleep(1);
}