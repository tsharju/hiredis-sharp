CC = mcs
SOURCEDIR = src
SOURCES := $(shell find $(SOURCEDIR) -name '*.cs')
LIB = build/LibHiredis.dll
EXAMPLE = build/Example.exe
PUBSUB = build/PubSub.exe
BENCHMARK = build/Benchmark.exe

all: $(LIB)

$(LIB): $(SOURCES)
	$(CC) -t:library $(SOURCES) -out:$(LIB)

$(EXAMPLE): $(LIB) examples/Example.cs
	$(CC) -r:$(LIB) examples/Example.cs -out:$(EXAMPLE)

$(PUBSUB): $(LIB) examples/PubSub.cs
	$(CC) -r:$(LIB) examples/PubSub.cs -out:$(PUBSUB)

$(BENCHMARK): $(LIB) benchmark/Benchmark.cs
	$(CC) -r:$(LIB),build/ServiceStack.Redis.dll,build/ServiceStack.Interfaces.dll benchmark/Benchmark.cs -out:build/Benchmark.exe

examples: $(EXAMPLE) $(PUBSUB)

benchmark: $(BENCHMARK)

clean:
	@rm -rf build/*.dll
	@rm -rf build/*.exe

.PHONY: all examples benchmark clean
