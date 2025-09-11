run:
	dotnet build && ./src/bin/Debug/net9.0/skakmat

clean:
	rm -rf src/bin/ src/obj/
