CC = mcs
SOURCEDIR = src
SOURCES := $(shell find $(SOURCEDIR) -name '*.cs')
LIB = build/LibHiredis.dll
EXE = build/Example.exe


all: $(LIB)

$(LIB): $(SOURCES)
	$(CC) -t:library $(SOURCES) -out:$(LIB)

$(EXE): $(LIB)
	$(CC) -r:$(LIB) examples/Example.cs -out:$(EXE)

example: $(EXE)

clean:
	@rm -rf build/*.dll
	@rm -rf build/*.exe

.PHONY: all example clean
