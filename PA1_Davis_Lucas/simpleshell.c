#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <sys/wait.h>
#include <unistd.h>
#include <fcntl.h>
#include <errno.h>
#include <sys/stat.h>

int executeCommand(char* cmd[]) {
    int status_code = 0;
    int pid, status;

    //Starts the fork prcess
    pid = fork();

    if(pid < 0) {
        //This will execute when the function fails to create a child process
        printf("fork Failed: %s\n", strerror(errno));
        status_code = -1;
        return status_code;
    } else if(pid == 0) {
        /* This is the child process
            will run the execvp which will take over the child process to run the given cmd */
        status_code = execvp(cmd[0], cmd);

        /* if the execvp returns -1, it means it was unsuccessful
            returns from the child process with a 1 status */
        if(status_code == -1){
            printf("exec Failed: %s\n", strerror(errno));
            _exit(status_code);                    //terminates the calling process "immediately"
        }
    } else {
        /*This is the parent proces of the fork function
            suspends the calling porcess until the system gets status information on the child
            WUNTRACED is the status information option that we want to looking for:
            this option will report on stopped and terminated child processes */
        waitpid(pid, &status, WUNTRACED);
    }

    return status_code;
}

int changeDirectories(char* path[], int _size) {
    int status_code;

    //if there isn't two elements including the null character, return error.
    if(_size != 2) {
        printf("Path Not Formatted Correctly!\n");
    } else {
        //changes cwd of calling process to directory specified in path
        status_code = chdir(path[1]);

        //-1 returned on error
        if(status_code == -1){
            printf("chdir Failed: %s\n", strerror(errno));
            return status_code;
        }
    }

    return 0;
}

// This function takes a cmd and splits it into usable tokens
int parseInput(char *str, char* _tokens[]) {
    const char s[2] = " "; // character to look for in the strtok cmd
    char* tmp;
    int i = 0;

    // get the first token
    tmp = strtok(str, s);

    /* Walk through each token and assign them to a character array
       in the main function */
    while (tmp != NULL) {
        _tokens[i++] = tmp;
        
        /* NULL tells strtok to continue tokenizing the first string
           We passed in */
        tmp = strtok(NULL, s);
    }

    _tokens[i] = NULL;

    return i;   //return the size of the tokens array
}

int main(int argc, char *argv[]) {
    char cmd[30] = "";      //command
    char cwd[256];          //current working directory
    char* tokens[200];
    int size;
    
    do {
        /* returns a null-terminated string of the absolute pathname of calling process on success
           if length of path is greater then size bytes, NULL is returned.*/
        if(getcwd(cwd, sizeof(cwd)) == NULL)
            printf("getcwd() error: %s\n", strerror(errno));

        printf("lucasdavis:%s$ ", cwd);

        /* reads a specified stream until it encounters (n-1) characters
           or a newline character. Needed to read whitespace in the cmd */
        fgets(cmd, sizeof(cmd), stdin);
        cmd[strcspn(cmd, "\n")] = 0;        //Clears the \n at the end of the cli string

        // Parse input into usable tokens, and save the size of the tokens array
        size = parseInput(cmd, tokens);


        if(strcmp(tokens[0], "cd") == 0) changeDirectories(tokens, size);
        else if(strcmp(tokens[0], "exit") == 0) break;
        else executeCommand(tokens);
    } while(1);

    return 0;
}