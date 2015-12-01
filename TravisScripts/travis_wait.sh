travis_wait() {
  local timeout=$1

  if [[ $timeout =~ ^[0-9]+$ ]]; then
    # looks like an integer, so we assume it's a timeout
    shift
  else
    # default value
    timeout=20
  fi

  travis_wait_impl $timeout "$@"
}

travis_wait_impl() {
  local timeout=$1
  shift

  local cmd="$@"
  local log_file=travis_wait_$$.log

  $cmd 2>&1 >$log_file &
  local cmd_pid=$!

  travis_jigger $! $timeout $cmd &
  local jigger_pid=$!
  local result

  {
    wait $cmd_pid 2>/dev/null
    result=$?
    ps -p$jigger_pid 2>&1>/dev/null && kill $jigger_pid
  } || return 1

  echo -e "\n\033[32;1mDone\033[0m, exited with $result\n"

  return $result
}

travis_jigger() {
  # helper method for travis_wait()
  local cmd_pid=$1
  shift
  local timeout=$1 # in minutes
  shift
  local count=0


  # clear the line
  echo -ne "."

  while [ $count -lt $timeout ]; do
    count=$(($count + 1))
    echo -ne "."
    sleep 30
  done

  echo -e "\n\033[31;1mTimeout (${timeout} minutes) reached. Terminating \"$@\"\033[0m\n"
  kill -9 $cmd_pid
}
