import http from 'k6/http';
import { sleep } from 'k6';

const API_URL = 'https://localhost:7073/api/polly/200';

export const options = {
    vus: 1, // Estou simulando 1 usuario
    duration: '10s', // durante 10 segundos
    thresholds: {
        http_req_duration: ['p(95)<500'] // Espero que 95% das chamadas estejam abaixo de 500 milisegundos 
    },
};

export default function () {
    http.get(API_URL);
    sleep(1);
}