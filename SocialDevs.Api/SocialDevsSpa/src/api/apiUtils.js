export async function handleResponse(response) {

    if (response.ok) return response.json();
    if (response.status === 400) {
      // So, a server-side validation error occurred.
      // Server side validation returns a string error message, so parse as text instead of json.
      const error = await response.text();
      throw new Error(error);
    }
    throw new Error("Network response was not ok.");
  }
  
  // In a real app, would likely call an error logging service.
  export function handleError(error) {
    // eslint-disable-next-line no-console
    console.error("API call failed. " + error);
    throw error;
  }


  // fetch("api/post/getallposts", {
  //   method: "GET",
  //   headers: {
  //     Accept: "application/json",
  //     "Content-Type": "application/json"
  //   }
  // })
  //   .then(result => result.json())
  //   .then(data => {
  //     console.log(data.result);

  //     let val = data.result;

  //     this.setState({ getValue: val });
  //     console.log("güncel state", this.state.getValue);
  //   });
  