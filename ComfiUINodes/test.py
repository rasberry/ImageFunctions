import os

PIPE_NAME = "IF3b17977d-b150-4e57-8ad9-ad6520f54ba3"
# pr,pw = os.mkfifo(PIPE_NAME)
pipe = open(f'\\\\.\\PIPE\\{PIPE_NAME}', 'rb+', buffering=0)

pipe.write("this is a test".encode())
data = pipe.read().decode()
print(f'Read text: {data}')

pipe.close()
