import random
import socket
import time
from _thread import *
import threading
from datetime import datetime
import json
clients_lock = threading.Lock()  
clients = {} 
def connectionLoop(sock):
   while True:
      data, addr = sock.recvfrom(1024) 
      dataStr = str(data)
      CurrList = {"cmd": 3, "players": []}
      if addr in clients: 
         if 'heartbeat' in dataStr: 
          
            clients[addr]['lastBeat'] = datetime.now()
         else: 
            Locajson
             = json.loads(data) 
            clients[addr]['pos'] = {
            "x": Locajson
            ['pos']
            ['x'], "y": Locajson
            ['pos']
            ['y'], "z": Locajson
            ['pos']['z']} 
         else:
         if 'connect' in dataStr: 
            clients[addr] = {} 
            clients[addr]['lastBeat'] = datetime.now()
            clients[addr]['pos'] = 0 
            message = {"cmd": 0,"player":{"id":str(addr)}}
            m = json.dumps(message) 
            
            for c in clients:
            sock.sendto(bytes(m,'utf8'), (c[0],c[1])) 
            player = {} 
            player['id'] = str(c) 
           CurrList['players'].append(player) 
            j = json.dumps(CurrList) 
            sock.sendto(bytes(j,'utf8'), (addr[0],addr[1])) 
def cleanClients(sock):
   while True:
      for c in list(clients.keys()):
         if (datetime.now() - clients[c]['lastBeat']).total_seconds() > 5: 
            clients_lock.acquire()
            del clients[c]
            clients_lock.release()
            for cc in clients: 
               dropped = {"cmd": 2, "id":str(c)} 
               u = json.dumps(dropped)
               sock.sendto(bytes   u,'utf8'), (cc[0],cc[1]))
      time.sleep(1)
def gameLoop(sock): 
   while True:
      GameState = {"cmd": 1, "players": []} 
      clients_lock.acquire()
      for c in clients: 
         player = {} 
         player['pos'] = clients[c]['pos'] 
         player['id'] = str(c) 
         GameState['players'].append(player) 
      s=json.dumps(GameState)
      for c in clients:
         sock.sendto(bytes(s,'utf8'), (c[0],c[1])) 
      clients_lock.release() 
      time.sleep(.03) 
def main():
   port = 12345
   s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
   s.bind(('', port))
   start_new_thread(gameLoop, (s,))
   start_new_thread(connectionLoop, (s,))
   start_new_thread(cleanClients,(s,))
   while True:
      time.sleep(1) 

if __name__ == '__main__': 
 main()
