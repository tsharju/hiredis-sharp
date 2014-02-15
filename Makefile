CC = mcs
SOURCEDIR = src
EXAMPLEDIR = examples
SOURCES := $(shell find $(SOURCEDIR) -name '*.cs')
EXAMPLES := $(shell find $(EXAMPLEDIR) -name '*.cs') 
LIB = build/LibHiredis.dll
EXE = build/Example.exe
BENCHMARK = build/Benchmark.exe


all: $(LIB)

$(LIB): $(SOURCES)
	$(CC) -t:library $(SOURCES) -out:$(LIB)

$(EXE): $(LIB) examples/Example.cs
	$(CC) -r:$(LIB) examples/Example.cs -out:$(EXE)

$(BENCHMARK): $(LIB) examples/Benchmark.cs
	$(CC) -r:$(LIB),build/ServiceStack.Redis.dll,build/ServiceStack.Interfaces.dll examples/Benchmark.cs -out:$(BENCHMARK)

example: $(EXE)

benchmark: $(BENCHMARK)

clean:
	@rm -rf build/*.dll
	@rm -rf build/*.exe

.PHONY: all example benchmark clean
