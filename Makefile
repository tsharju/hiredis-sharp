CC = mcs
SOURCEDIR = src
EXAMPLEDIR = examples
SOURCES := $(shell find $(SOURCEDIR) -name '*.cs')
EXAMPLES := $(shell find $(EXAMPLEDIR) -name '*.cs') 
LIB = build/LibHiredis.dll
EXE = build/Example.exe


all: $(LIB)

$(LIB): $(SOURCES)
	$(CC) -t:library $(SOURCES) -out:$(LIB)

$(EXE): $(LIB) $(EXAMPLES)
	$(CC) -r:$(LIB) $(EXAMPLES) -out:$(EXE)

example: $(EXE)

clean:
	@rm -rf build/*.dll
	@rm -rf build/*.exe

.PHONY: all example clean
