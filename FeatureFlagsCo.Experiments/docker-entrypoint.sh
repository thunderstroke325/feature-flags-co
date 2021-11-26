#!/bin/sh

# Abort on any error (including if wait-for-it fails).
set -e

# Wait for the backend to be up, if we know where it is.
./wait-for-it.sh "${CUSTOMERS_HOST:-redis}:${CUSTOMERS_PORT:-6379} --timeout=300 --strict" 


# Run the main container command.
exec "$@"