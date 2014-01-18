all:
	@gmcs -t:library src/LibHiredis.cs src/RedisClient.cs -out:build/Hiredis.dll

clean:
	@rm -rf build/*.dll
