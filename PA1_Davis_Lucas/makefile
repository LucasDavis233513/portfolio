TARGET = simpleshell
CC = gcc
CFLAGS = -g -Wall

default: 
	$(CC) $(CFLAGS) $(TARGET).c -o $(TARGET)

all:
	%: %.c
		$(CC) $(CFLAGS) $.c -o $*

clean:
	rm -f $(TARGET)