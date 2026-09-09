get-cert:
	dotnet dev-certs https -ep certs/root.pfx -p SOLOMONKEY_BLUD
	dotnet dev-certs https --trust