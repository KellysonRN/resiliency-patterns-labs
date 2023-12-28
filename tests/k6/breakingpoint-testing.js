import http from 'k6/http';
import { sleep } from 'k6';

const API_URL = 'https://localhost:7073/api/polly/200?type=cache';

export const options = {    
    executor: 'ramping-arrival-rate', //Garanta o aumento de carga se o sistema ficar lento.
    stages: [
      { duration: '2h', target: 20000 }, // Aumento gradual para uma carga ENORME.
    ],
  };

export default function () {
    http.get(API_URL);
    sleep(1);
}